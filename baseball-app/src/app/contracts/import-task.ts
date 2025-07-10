import { MediaImportTaskStatus } from './media-import-task-status';

export interface ImportTask {
    id: string;
    gameId: string;
    status: MediaImportTaskStatus;
    message: string;
    startTime?: Date;
    endTime?: Date;
    progress: number;
}
