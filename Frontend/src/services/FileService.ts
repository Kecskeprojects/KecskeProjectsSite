import FileData from "../models/FileData";
import ConvertTools from "../tools/ConvertTools";
import FileTools from "../tools/FileTools";
import BaseService from "./BaseService";

export default class FileService {
  static GetSingleFileEndpoint(
    identifier: string | undefined,
    folder: string | undefined
  ): string {
    identifier = identifier ? identifier : "";
    folder = folder ? folder : "";

    return `${BaseService.BackendRoute}/File/GetSingle/${identifier}?folder=${folder}`;
  }

  static async GetFileData(
    folder?: string | undefined
  ): Promise<Array<FileData>> {
    folder = folder ? folder : "";

    const rawDataList = await BaseService.Get(`/File/GetList?folder=${folder}`);
    return ConvertTools.ConvertListToType(FileData, rawDataList);
  }

  //Todo: Add additional file parameters to differentiate file uploads
  static async Upload(
    fileData: FormData,
    folder: string | undefined
  ): Promise<string | undefined> {
    folder = folder ? folder : "";

    const file = FileTools.getFileData(fileData);

    if (!file) {
      return;
    }

    const blobs = FileTools.getBlobChunksByLimit(file);

    let response = "";
    for (const blob of blobs) {
      const partialFormData = FileTools.getFileUploadData(fileData);
      partialFormData.append("file", blob);

      response += await BaseService.Post(
        `/File/Upload?folder=${folder}`,
        partialFormData
      );
    }

    return response;
  }
}
