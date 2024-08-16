from db_connect import Game
import os
import osxphotos
from osxphotos import PhotoInfo
from datetime import timedelta
from thumbnails import ThumbnailParams
from path_management import PathManager

class PhotoExporter:

    def __init__(self, path_manager: PathManager):
        self._paths = path_manager
        self.photosdb = osxphotos.PhotosDB()
        self.export_paths = dict()

    def export_photos_for_game(self, game: Game) -> str:
        photos = self.get_photos_for_game(game)
        for p in photos:
            self.export(p, game)

    def get_exported_photos_for_game(self, game: Game) -> list[PhotoInfo]:
        return [p for p in self.get_photos_for_game(game) if p.uuid in self.export_paths]

    def get_photos_to_thumbnail(self, game: Game) -> list[ThumbnailParams]:
        
        photos = self.get_exported_photos_for_game(game)
        if len(photos) == 0:
            return []
        
        preview_dir = self._paths.preview_dir(game)
        remaining_previews = set(os.listdir(preview_dir))
        
        to_thumbnail = []
        for photo in photos:
            export_path = self.export_paths[photo.uuid]
            ext = os.path.splitext(export_path)[1]
            preview_name = f'{photo.uuid}{ext}'
            if preview_name in remaining_previews:
                to_thumbnail.append(ThumbnailParams(export_path, photo.ismovie))
        
        return to_thumbnail
    
    def get_photos_to_upload(self, game: Game) -> list[PhotoInfo]:
        photos = self.get_exported_photos_for_game(game)
        if len(photos) == 0:
            return []
        
        preview_dir = self._paths.preview_dir(game)
        remaining_previews = set(os.listdir(preview_dir))
        
        to_upload: list[PhotoInfo] = []
        for photo in photos:
            export_path = self.export_paths[photo.uuid]
            ext = os.path.splitext(export_path)[1]
            preview_name = f'{photo.uuid}{ext}'
            if preview_name in remaining_previews:
                to_upload.append(photo)
        
        return to_upload

    def get_photos_for_game(self, game: Game) -> list[PhotoInfo]:
        start_time = game.StartTime + timedelta(hours=-3)
        end_time = game.EndTime + timedelta(hours=2)
        return self.photosdb.photos(from_date=start_time, to_date=end_time)
    
    def export(self, photo: PhotoInfo, game: Game):
        if photo.ismissing:
            print(f'skipping missing photo {photo.filename}')
            return
        
        out_dir = self._paths.temp_dir(game, photo)
        paths = photo.export(out_dir, live_photo=True, export_as_hardlink=True, overwrite=True)
        if len(paths) == 0:
            print(f'export issue with {photo}')
        else:
            self.export_paths[photo.uuid] = paths[0]
            ext = os.path.splitext(paths[0])[1]
            preview_dir = self._paths.preview_dir(game)
            photo.export(preview_dir, filename=f'{photo.uuid}{ext}', live_photo=False, export_as_hardlink=True, overwrite=True)
            