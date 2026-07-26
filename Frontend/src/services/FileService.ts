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
    targetPath?: string,
    identifier?: string,
  ): string {
    if (!identifier) {
      throw new Error("File identifier is required");
    }

    const route = BackendServiceTools.BuildWithTargetPath(
      `/File/GetSingle/`,
      targetPath,
      identifier,
    );

    return `${EnvironmentTools.getBackendRoute()}${route}`;
  }

  static async GetFileData(targetPath?: string): Promise<Array<FileData>> {
    const route = BackendServiceTools.BuildWithTargetPath(
      "/File/GetFileList",
      targetPath,
    );

    const rawDataList = await BaseService.Get(route);
    return ConvertTools.ConvertListToType(FileData, rawDataList?.content);
  }

  static async GetDirectoryData(
    targetPath?: string,
  ): Promise<Array<DirectoryData>> {
    const route = BackendServiceTools.BuildWithTargetPath(
      "/File/GetDirectoryList",
      targetPath,
    );

    const rawDataList = await BaseService.Get(route);
    return ConvertTools.ConvertListToType(DirectoryData, rawDataList?.content);
  }

  static async Upload(
    fileData: FormData,
    targetPath?: string,
    onUploadProgress?: (progressEvent: AxiosProgressEvent) => void,
  ): Promise<string | undefined> {
    const queryItems = {
      isNewFile: true,
    };

    const files = FileTools.getFileData(fileData);
    if (!files || files.length === 0) {
      return;
    }

    const route = BackendServiceTools.BuildWithTargetPath(
      "/File/Upload",
      targetPath,
    );

    for (const file of files) {
      const blobs = FileTools.getBlobChunksByLimit(file);

      for (const blob of blobs) {
        const partialFormData = FileTools.getFileUploadData(fileData, file);
        partialFormData.append("file", blob);

        const response = await BaseService.Post(
          route,
          queryItems,
          partialFormData,
          onUploadProgress,
        );

        if (response.message != "Success!") {
          return "Error during file upload!";
        }

        queryItems.isNewFile = false;
      }
    }

    return "File uploaded successfully!";
  }
}
