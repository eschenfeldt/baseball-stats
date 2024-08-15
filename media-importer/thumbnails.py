import os
from dataclasses import dataclass
from PIL import Image
import subprocess
from multiprocessing import Pool
from pillow_heif import register_heif_opener
register_heif_opener()

@dataclass
class ThumbnailDef:
    max_size: int
    name_modifier: str

@dataclass
class ThumbnailParams:
    path: str
    ismovie: bool

class Thumbnailer:

    def __init__(self, sizes: list[ThumbnailDef]):
        # sizes will be computed sequentially, so order from large to small
        self.sizes = sorted(sizes, key=lambda s: s.max_size, reverse=True)
        self.name_modifiers = [size.name_modifier for size in sizes]

    def thumbnail_many(self, params:list[ThumbnailParams]):
        with Pool(12) as pool:
            results = pool.map(self.thumbnail, params)
        for result in results:
            if result:
                print('Error: ', result)

    def thumbnail(self, params: ThumbnailParams):
        if params.ismovie:
            return self.thumbnail_video(params.path)
        else:
            return self.thumbnail_photo(params.path)

    def thumbnail_photo(self, path: str):
        try:
            dir = os.path.dirname(path)
            filename, ext = os.path.splitext(os.path.basename(path))
            image = Image.open(path)
            for size in self.sizes:
                image.thumbnail((size.max_size, size.max_size))
                new_name = os.path.join(dir, f"{filename}_{size.name_modifier}{ext}")
                image.save(new_name)
        except Exception as e:
            return e
        
    def thumbnail_video(self, path:str):
        try:
            dir = os.path.dirname(path)
            filename, _ = os.path.splitext(os.path.basename(path))
            ext = '.jpeg'
            new_name = os.path.join(dir, f"{filename}{ext}")
            subprocess.call(['ffmpeg', '-i', path, '-ss', '00:00:00.000', '-vframes', '1', new_name, '-y', '-hide_banner', '-loglevel', 'error'])
            image = Image.open(new_name)
            for size in self.sizes:
                image.thumbnail((size.max_size, size.max_size))
                new_name = os.path.join(dir, f"{filename}_{size.name_modifier}{ext}")
                image.save(new_name)
        except Exception as e:
            return e        