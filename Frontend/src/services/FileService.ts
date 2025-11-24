import type { AxiosProgressEvent } from "axios";
import FileData from "../models/FileData";
import BackendServiceTools from "../tools/BackendServiceTools";
import ConvertTools from "../tools/ConvertTools";
import EnvironmentTools from "../tools/EnvironmentTools";
import FileTools from "../tools/FileTools";
import BaseService from "./BaseService";

export default class FileService {
  static GetSingleFileEndpoint(identifier?: string, folder?: string): string {
    identifier = BackendServiceTools.SanitizeQueryParameter(identifier);
    folder = BackendServiceTools.SanitizeQueryParameter(folder);

    return `${EnvironmentTools.getBackendRoute()}/File/GetSingle/${identifier}?folder=${folder}`;
  }

  static async GetFileData(folder?: string): Promise<Array<FileData>> {
    const queryItems = {
      folder: folder,
    };

    const rawDataList = await BaseService.Get("/File/GetList", queryItems);
    return ConvertTools.ConvertListToType(FileData, rawDataList?.content);
  }

  static async Upload(
    fileData: FormData,
    folder?: string,
    onUploadProgress?: (progressEvent: AxiosProgressEvent) => void
  ): Promise<string | undefined> {
    const queryItems = {
      folder: folder,
      newFile: true,
    };

    const files = FileTools.getFileData(fileData);

    if (!files || files.length === 0) {
      return;
    }

    for (const file of files) {
      const blobs = FileTools.getBlobChunksByLimit(file);

      for (const blob of blobs) {
        const partialFormData = FileTools.getFileUploadData(fileData, file);
        partialFormData.append("file", blob);

        const response = await BaseService.Post(
          "/File/Upload",
          queryItems,
          partialFormData,
          onUploadProgress
        );

        if (response.message != "Success!") {
          return "Error during file upload!";
        }

        queryItems.newFile = false; //This is a flag to help check for existing files before uploading
      }
    }

    return "File uploaded successfully!";
  }
}
