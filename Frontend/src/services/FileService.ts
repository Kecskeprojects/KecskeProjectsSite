import FileData from "../models/FileData";
import ConvertTools from "../tools/ConvertTools";
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
}
