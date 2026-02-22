export default class DirectoryData {
  Name: string;
  CreatedAtUtc: Date;
  SubPath: string;

  constructor(data: any) {
    this.Name = data.name;
    this.CreatedAtUtc = data.createdAtUtc;
    this.SubPath = data.subPath;
  }
}
