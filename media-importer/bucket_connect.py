from dataclasses import dataclass
import json
import boto3
import boto3.exceptions
import boto3.session
import requests
from osxphotos import PhotoInfo
import os
from path_management import PathManager

@dataclass
class BucketConfig:
    region: str
    endpoint: str
    bucket: str
    access_key: str
    secret_key: str

class BucketConnector:

    def __init__(self, path: str, path_manager: PathManager):
        self.path_manager = path_manager
        with open(path, 'r') as config_file:
            config_dict = json.loads(config_file.read())
        self.__config = BucketConfig(**config_dict)

    def get_client(self):
        return boto3.client('s3',
                              region_name=self.__config.region,
                              endpoint_url=self.__config.endpoint,
                              aws_access_key_id=self.__config.access_key,
                              aws_secret_access_key=self.__config.secret_key)

    def get_file_purpose(self, name_modifier: str | None, ext: str, photo: PhotoInfo) -> str:
        if photo.ismovie and ext == '.jpeg':
            return 'thumbnail'
        elif name_modifier is not None:
            return 'thumbnail'
        else:
            return 'original'

    def get_key(self, photo: PhotoInfo, name_modifier: str | None, ext: str) -> str:
        base = self.get_file_purpose(name_modifier, ext, photo)
        if name_modifier:
            base = f'{base}_{name_modifier}'
        return f'{photo.uuid}/{base}{ext}'

    def file_exists(self, photo: PhotoInfo, name_modifier: str, ext: str) -> bool:
        key = self.get_key(photo, name_modifier, ext)
        return self.file_exists_by_key(key)
    
    def file_exists_by_key(self, key: str) -> bool:
        base = f'https://{self.__config.bucket}.{self.__config.region}.digitaloceanspaces.com'
        request = requests.head(f'{base}/{key}')
        return request.status_code == 200
    
    def upload_by_key(self, path: str, key: str):
        client = self.get_client()
        client.upload_file(path, self.__config.bucket, key, ExtraArgs={'ACL':'public-read'})

    def upload_file(self, path: str, photo: PhotoInfo, name_modifier: str, ext: str):
        key = self.get_key(photo, name_modifier, ext)
        self.upload_by_key(path, key)

    def upload_all_files(self, root_path: str, photo: PhotoInfo):
        for file in os.listdir(root_path):
            name, ext = os.path.splitext(file)
            name_modifier = self.path_manager.get_name_modifier(name)
            if not self.file_exists(photo, name_modifier, ext):
                full_path = os.path.join(root_path, file)
                self.upload_file(full_path, photo, name_modifier, ext)