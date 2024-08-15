import os
from dataclasses import dataclass
from datetime import date, datetime
from osxphotos import PhotoInfo

@dataclass
class Game:
    Id: int
    Name: str
    Date: date
    ScheduledTime: datetime
    StartTime: datetime
    EndTime: datetime 

class PathManager:

    def __init__(self):
        self.root = os.path.join('.', 'exported')

    def base_dir(self, game: Game) -> str:
        safe_name = game.Name.replace(os.path.sep, '_')
        return os.path.join('.', 'exported', safe_name)

    def temp_dir(self, game: Game, photo: PhotoInfo) -> str:
        base_dir = self.base_dir(game)
        out_dir = os.path.join(base_dir, photo.uuid)
        if not os.path.exists(out_dir):
            os.makedirs(out_dir)
        return out_dir

    def preview_dir(self, game: Game) -> str:
        base_dir = self.base_dir(game)
        out_dir = os.path.join(base_dir, 'preview')
        if not os.path.exists(out_dir):
            os.makedirs(out_dir)
        return out_dir