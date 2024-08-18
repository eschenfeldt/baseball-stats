from dataclasses import dataclass
import json
import psycopg
from psycopg.rows import dict_row
from path_management import Game, PathManager
from osxphotos import PhotoInfo
import os
from datetime import date

@dataclass
class PostgresConfig:
    hostname: str
    database: str
    username: str
    password: str

    def connection_info(self) -> str:
        return f'postgresql://{self.username}:{self.password}@{self.hostname}/{self.database}'


class DbConnector:

    def __init__(self, path: str, path_manager: PathManager):
        self._path_manager = path_manager
        with open(path, 'r') as config_file:
            config_dict = json.loads(config_file.read())
        self.__config = PostgresConfig(**config_dict)


    def get_games(self, from_date: date = None, to_date: date = None) -> list[Game]:
        with psycopg.connect(self.__config.connection_info()) as connection:
            with connection.cursor(row_factory=dict_row) as cursor:
                games_q = cursor.execute('SELECT "Id", "Name", "Date", "ScheduledTime", "StartTime", "EndTime" FROM "Games"')
                games = games_q.fetchall()
                results = [Game(**g) for g in games]
        
        if from_date:
            results = [g for g in results if g.Date >= from_date]
        if to_date:
            results = [g for g in results if g.Date <= to_date]

        return results
    
    def get_game_by_id(self, id: int) -> Game:
        statement = 'SELECT "Id", "Name", "Date", "ScheduledTime", "StartTime", "EndTime" FROM "Games" WHERE "Id" = %s'
        with psycopg.connect(self.__config.connection_info()) as connection:
            with connection.cursor(row_factory=dict_row) as cursor:
                games_q = cursor.execute(statement, (id,))
                g = games_q.fetchone()
                return Game(**g)
    
    def get_game_by_name(self, name: str) -> int | None:
        statement = """
        SELECT "Id"
        FROM "Games"
        WHERE "Games"."Name" = %s
        """
        with psycopg.connect(self.__config.connection_info()) as connection:
            with connection.cursor() as cursor:
                result = cursor.execute(statement, (name,))
                id = result.fetchone()
                if id:
                    return id[0]
                else:
                    return None
                
    def get_game_by_date(self, date: date) -> int | None:
        statement = """
        SELECT "Id"
        FROM "Games"
        WHERE "Games"."Date" = %s
        """
        with psycopg.connect(self.__config.connection_info()) as connection:
            with connection.cursor() as cursor:
                result = cursor.execute(statement, (date,))
                ids = result.fetchall()
                if ids and len(ids) == 1:
                    # if there are multiple games on the same day don't return any of them
                    return ids[0][0]
                else:
                    return None
                
    def has_scorecard(self, game_id) -> bool:
        statement = """
        SELECT "ScorecardId"
        FROM "Games"
        WHERE "Games"."Id" = %s
        """
        with psycopg.connect(self.__config.connection_info()) as connection:
            with connection.cursor() as cursor:
                result = cursor.execute(statement, (game_id,))
                id = result.fetchone()
                if id:
                    return id[0] is not None
                else:
                    return False

    def get_db_id(self, photo_uuid: str) -> int | None:
        statement = """
        SELECT "Id"
        FROM "RemoteResource"
        WHERE "RemoteResource"."AssetIdentifier" = %s
        """
        with psycopg.connect(self.__config.connection_info()) as connection:
            with connection.cursor() as cursor:
                result = cursor.execute(statement, (photo_uuid,))
                id = result.fetchone()
                if id:
                    return id[0]
                else:
                    return None
            
    def file_exists(self, photo_uuid: str, name_modifier: str, ext: str) -> bool:
        statement = """
        SELECT COUNT(*)
        FROM "RemoteResource"
        JOIN "RemoteFile" ON "RemoteResource"."Id" = "RemoteFile"."ResourceId"
        WHERE "RemoteResource"."AssetIdentifier" = %s
            AND "RemoteFile"."NameModifier" = %s
            AND "RemoteFile"."Extension" = %s
        """
        with psycopg.connect(self.__config.connection_info()) as connection:
            with connection.cursor() as cursor:
                count = cursor.execute(statement, (photo_uuid, name_modifier, ext))
                return count.fetchone()[0] > 0
            
    def resource_type(self, photo: PhotoInfo) -> int:
        if photo.ismovie:
            return 4
        elif photo.live_photo:
            return 3
        elif photo.isphoto:
            return 2
        else:
            return 0
    
    def get_params(self, gameId: int, photo: PhotoInfo) -> tuple:
        return (
            photo.uuid, 
            photo.date, 
            photo.original_filename, 
            gameId, 
            'MediaResource', 
            self.resource_type(photo),
            photo.favorite
        )

    def get_file_purpose(self, name_modifier: str | None, ext: str, photo: PhotoInfo) -> int:
        if photo.ismovie and ext == '.jpeg':
            return 2 # Thumbnail
        elif name_modifier is not None:
            return 2 # Thumbnail
        else:
            return 1 # Original

    def get_file_params(self, resourceId: int, game: Game, photo: PhotoInfo) -> list[tuple]:
        out_dir = self._path_manager.temp_dir(game, photo)
        results = []
        for file in os.listdir(out_dir):
            name, ext = os.path.splitext(os.path.basename(file))
            name_modifier = self._path_manager.get_name_modifier(name)
            file_purpose = self.get_file_purpose(name_modifier, ext, photo)
            
            results.append((resourceId, file_purpose, name_modifier, ext))
        return results
    

    def import_files(self, resourceId: int, game: Game, photo: PhotoInfo, cursor):
        statement = """
            INSERT INTO "RemoteFile"("ResourceId", 
                                        "Purpose", 
                                        "NameModifier", 
                                        "Extension") 
            VALUES (%s, %s, %s, %s)
        """
        for params in self.get_file_params(resourceId, game, photo):
            if not self.file_exists(photo.uuid, params[2], params[3]):
                cursor.execute(statement, params)

    def import_resources(self, game: Game, photos: list[PhotoInfo]):
        statement = """
            INSERT INTO "RemoteResource"("AssetIdentifier", 
                                        "DateTime", 
                                        "OriginalFileName", 
                                        "GameId", 
                                        "Discriminator", 
                                        "ResourceType", 
                                        "Favorite") 
            VALUES (%s, %s, %s, %s, %s, %s, %s)
            RETURNING "Id"
        """
        with psycopg.connect(self.__config.connection_info()) as connection:
            with connection.cursor() as cursor:
                for photo in photos:
                    id = self.get_db_id(photo.uuid)
                    if id is None:
                        params = self.get_params(game.Id, photo)
                        cursor.execute(statement, params)
                        id = cursor.fetchone()[0]
                    self.import_files(id, game, photo, cursor)
            
            connection.commit()

    def import_scorecard_file(self, resource_id: int, ext: str, cursor):
        statement = """
            INSERT INTO "RemoteFile"("ResourceId", 
                                        "Purpose", 
                                        "NameModifier", 
                                        "Extension") 
            VALUES (%s, %s, %s, %s)
        """
        params = (resource_id, 1, None, ext)
        cursor.execute(statement, params)

    def import_scorecard(self, game: Game, identifier: str, original_name: str, ext: str):
        statement = """
            INSERT INTO "RemoteResource"("AssetIdentifier", 
                                        "DateTime", 
                                        "OriginalFileName", 
                                        "GameId", 
                                        "Discriminator", 
                                        "ResourceType", 
                                        "Favorite") 
            VALUES (%s, %s, %s, %s, %s, %s, %s)
            RETURNING "Id"
        """
        update_statement = """
        UPDATE "Games"
        SET "ScorecardId" = %s
        WHERE "Id" = %s
        """
        with psycopg.connect(self.__config.connection_info()) as connection:
            with connection.cursor() as cursor:
                params = (identifier, game.EndTime, original_name, game.Id, 'Scorecard', 1, False)
                cursor.execute(statement, params)
                id = cursor.fetchone()[0]
                self.import_scorecard_file(id, ext, cursor)
                cursor.execute(update_statement, (id, game.Id))
