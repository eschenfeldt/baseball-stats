import { FileType } from "./file-type"
import { ThumbnailSize } from "./thumbnail-size"

export interface RemoteFileDetail {
    assetIdentifier: string
    dateTime: string
    fileType: FileType
    remoteFilePurpose: number
    nameModifier?: ThumbnailSize
    extension: string
    originalFileName: string
    key: string
}
