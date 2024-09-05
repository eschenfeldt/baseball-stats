import { FileType } from "./file-type";
import { RemoteFileDetail } from "./remote-file-detail";

export interface RemoteOriginal {

    fileType: FileType;
    gameName?: string;
    photo: RemoteFileDetail;
    video: RemoteFileDetail;
    alternatePhoto?: RemoteFileDetail;
    alternateVideo?: RemoteFileDetail;
}
