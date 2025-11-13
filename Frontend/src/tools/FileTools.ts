export default class FileTools {
  static getBlobChunksByLimit(file: File | Blob): Array<Blob> {
    const gbLimit = FileTools.getFileSizeLimit();
    if (isNaN(gbLimit)) {
      throw Error("File upload size limit set incorrectly");
    }

    const size = file.size;

    const byteLimit = FileTools.getGBInBytes(gbLimit);

    const blobs = new Array<Blob>();
    const chunkCount = Math.ceil(file.size / byteLimit);

    if (chunkCount == 1) {
      return [file];
    }

    for (let i = 0; i < chunkCount; i++) {
      const start = i * byteLimit;
      const end = (i + 1) * byteLimit;

      const realEnd = size < end ? size : end;

      blobs.push(file.slice(start, realEnd, file.type));
    }

    return blobs;
  }

  static getFileSizeLimit(): number {
    const value = import.meta.env.VITE_MAX_FILE_UPLOAD_SIZE_IN_GB;

    return parseFloat(value);
  }

  static getGBInBytes(gb: number): number {
    return gb * 1024.0 * 1024.0 * 1024.0;
  }

  static getFileData(formData: FormData): Blob | undefined {
    for (const data of formData) {
      if (data[1] instanceof Blob) {
        return data[1];
      }
    }
  }

  static getFileUploadData(formData: FormData): FormData {
    const newFormData = new FormData();
    for (const data of formData) {
      if (data[1] instanceof Blob) {
        continue;
      }
      newFormData.append(data[0], data[1]);
    }
    return newFormData;
  }
}
