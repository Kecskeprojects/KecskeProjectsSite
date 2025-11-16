import EnvironmentTools from "./EnvironmentTools";

export default class FileTools {
  static getBlobChunksByLimit(file: File): Array<Blob> {
    const gbLimit = EnvironmentTools.getFileSizeLimit();
    const byteLimit = FileTools.getGBInBytes(gbLimit);

    const blobs = new Array<Blob>();
    const chunkCount = Math.ceil(file.size / byteLimit);

    if (chunkCount == 1) {
      return [file];
    }

    const size = file.size;
    for (let i = 0; i < chunkCount; i++) {
      const start = i * byteLimit;
      const end = (i + 1) * byteLimit;

      const realEnd = size < end ? size : end;

      blobs.push(file.slice(start, realEnd, file.type));
    }

    return blobs;
  }

  static getGBInBytes(gb: number): number {
    return gb * 1024.0 * 1024.0 * 1024.0;
  }

  static getFileData(formData: FormData): Array<File> | undefined {
    const files = [];
    for (const data of formData) {
      if (data[1] instanceof File) {
        files.push(data[1]);
      }
    }
    return files;
  }

  static getFileUploadData(formData: FormData, file: File): FormData {
    const newFormData = new FormData();
    newFormData.append("fileName", file.name);
    newFormData.append("totalSize", file.size.toString());

    for (const data of formData) {
      if (data[1] instanceof File) {
        continue;
      }
      newFormData.append(data[0], data[1]);
    }

    return newFormData;
  }
}
