import FileData from "../models/FileData";
import ConvertTools from "../tools/ConvertTools";
import BaseService from "./BaseService";

export default class FileService {
  static FileResponseEndpoint: string = `${BaseService.BackendRoute}/File/GetSingle`;

  static async GetFileData(
    folder?: string | undefined
  ): Promise<Array<FileData>> {
    const rawDataList = await BaseService.Get(
      `/File/GetList?folder=${folder ?? ""}`
    );
    return ConvertTools.ConvertListToType(FileData, rawDataList);
  }
}
