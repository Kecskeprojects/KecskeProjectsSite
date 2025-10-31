import FileTypeEnum from "../enum/FileTypeEnum";

export default class FileData {
  Type: string;
  Extension: string;
  IsFolder: boolean;
  Name: string;
  SizeInMb: number;
  SizeInGb: number;
  CreatedAtUtc: Date;
  Identifier: string;
  Folder: string;

  constructor(data: any) {
    this.Name = data.name;
    this.SizeInMb = data.sizeInMb;
    this.SizeInGb = data.sizeInGb;
    this.CreatedAtUtc = data.createdAtUtc;
    this.Identifier = data.identifier;
    this.Extension = data.extension;
    this.IsFolder = data.isFolder;
    this.Folder = data.folder;

    switch (data.extension) {
      case ".mp4":
      case ".mkv":
        this.Type = FileTypeEnum.Video;
        break;
      case ".png":
      case ".jpg":
        this.Type = FileTypeEnum.Image;
        break;
      case ".txt":
        this.Type = FileTypeEnum.Other;
        break;
      default:
        this.Type = FileTypeEnum.Unknown;
        break;
    }
  }
}
