import FileData from "../models/FileData";
import ConvertTools from "../tools/ConvertTools";
import BaseService from "./BaseService";

export default class FileService {
  static async GetFileData(): Promise<Array<FileData>> {
    const rawDataList = await BaseService.Get("/File/GetList");
    return ConvertTools.ConvertListToType(FileData, rawDataList);
  }
}
