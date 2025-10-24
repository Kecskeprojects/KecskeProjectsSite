import FileTypeEnum from "../enum/FileTypeEnum";

export default class FileData {
  Type: FileTypeEnum;
  Extension: string;
  IsFolder: boolean;
  Name: string;
  SizeMb: number;
  SizeGb: number;
  CreatedAt: Date;
  RelativeRoute: string; //Todo: This has to be modified to contain as little information on folder structure as possible, multiple variables, naming, etc.

  constructor(data: any) {
    this.Name = data.name;
    this.SizeMb = data.sizeMb;
    this.SizeGb = data.sizeMb / 1024.0;
    this.CreatedAt = data.createdAt;
    this.RelativeRoute = data.relativeRoute;
    this.Extension = data.extension;
    this.IsFolder = data.isFolder;

    switch (data.extension) {
      case ".mp4":
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
