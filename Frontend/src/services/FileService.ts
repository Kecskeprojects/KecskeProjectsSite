import type { AxiosProgressEvent } from "axios";
import DirectoryData from "../models/DirectoryData";
import FileData from "../models/FileData";
import BackendServiceTools from "../tools/BackendServiceTools";
import ConvertTools from "../tools/ConvertTools";
import EnvironmentTools from "../tools/EnvironmentTools";
import FileTools from "../tools/FileTools";
import BaseService from "./BaseService";

export default class FileService {
  static GetSingleFileEndpoint(
    category?: string,
    subPath?: string,
    identifier?: string,
  ): string {
    category = BackendServiceTools.SanitizeQueryParameter(category);
    subPath = BackendServiceTools.SanitizeQueryParameter(subPath);
    identifier = BackendServiceTools.SanitizeQueryParameter(identifier);

    return `${EnvironmentTools.getBackendRoute()}/File/GetSingle/${identifier}?category=${category}&subPath=${subPath}`;
  }

  static async GetFileData(
    category?: string,
    subpath?: string,
  ): Promise<Array<FileData>> {
    const queryItems = { category, subpath };

    const rawDataList = await BaseService.Get("/File/GetFileList", queryItems);
    return ConvertTools.ConvertListToType(FileData, rawDataList?.content);
  }

  static async GetDirectoryData(
    category?: string,
    subpath?: string,
  ): Promise<Array<DirectoryData>> {
    const queryItems = { category, subpath };

    const rawDataList = await BaseService.Get(
      "/File/GetDirectoryList",
      queryItems,
    );
    return ConvertTools.ConvertListToType(DirectoryData, rawDataList?.content);
  }

  static async Upload(
    fileData: FormData,
    folder?: string,
    onUploadProgress?: (progressEvent: AxiosProgressEvent) => void,
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
          onUploadProgress,
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
