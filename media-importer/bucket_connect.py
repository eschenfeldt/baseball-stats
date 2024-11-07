from dataclasses import dataclass
import json
import boto3
import boto3.exceptions
import boto3.session
import requests
from osxphotos import PhotoInfo
import os
from path_management import PathManager
from multiprocessing import Pool

@dataclass
class BucketConfig:
    region: str
    endpoint: str
    bucket: str
    access_key: str
    secret_key: str

@dataclass
class UploadParams:
    override: bool
    key: str
    file_path: str

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

    def get_file_purpose(self, name_modifier: str | None, ext: str, photo: PhotoInfo, has_alternate_formats: bool) -> str:
        if photo.ismovie and ext == '.jpeg':
            return 'thumbnail'
        elif name_modifier is not None:
            return 'thumbnail'
        elif has_alternate_formats and ext == '.jpeg':
            return 'alt'
        else:
            return 'original'

    def get_key(self, photo: PhotoInfo, name_modifier: str | None, ext: str, has_alternate_formats: bool) -> str:
        base = self.get_file_purpose(name_modifier, ext, photo, has_alternate_formats)
        if name_modifier:
            base = f'{base}_{name_modifier}'
        return f'{photo.uuid}/{base}{ext}'

    def file_exists(self, photo: PhotoInfo, name_modifier: str, ext: str, has_alternate_formats: bool) -> bool:
        key = self.get_key(photo, name_modifier, ext, has_alternate_formats)
        return self.file_exists_by_key(key)
    
    def file_exists_by_key(self, key: str) -> bool:
        base = f'https://{self.__config.bucket}.{self.__config.region}.digitaloceanspaces.com'
        request = requests.head(f'{base}/{key}')
        return request.status_code == 200
    
    def upload_by_key(self, path: str, key: str):
        client = self.get_client()
        client.upload_file(path, self.__config.bucket, key, ExtraArgs={'ACL':'public-read'})

    def upload_file(self, path: str, photo: PhotoInfo, name_modifier: str, ext: str, has_alternate_formats: bool):
        key = self.get_key(photo, name_modifier, ext, has_alternate_formats)
        self.upload_by_key(path, key)

    def get_upload_params(self, root_path: str, photo: PhotoInfo, override: bool = False):
        original_formats_count = 0
        for file in os.listdir(root_path):
            name, ext = os.path.splitext(file)
            name_modifier = self.path_manager.get_name_modifier(name)
            if name_modifier is None:
                original_formats_count += 1

        has_alternate_formats = original_formats_count > 1

        params: list[UploadParams] = []

        for file in os.listdir(root_path):
            name, ext = os.path.splitext(file)
            name_modifier = self.path_manager.get_name_modifier(name)
            key = self.get_key(photo, name_modifier, ext, has_alternate_formats)
            full_path = os.path.join(root_path, file)
            params.append(UploadParams(override, key, full_path))

        return params

    def upload_many_files(self, params: list[UploadParams], pool_size=12):
        with Pool(pool_size) as pool:
            results = pool.map(self.upload_parallelizable, params)
        for result in results:
            if result:
                print('Error: ', result)

    def upload_parallelizable(self, params: UploadParams):
        try:
            if params.override or not self.file_exists_by_key(params.key):
                self.upload_by_key(params.file_path, params.key)
        except Exception as e:
            return e