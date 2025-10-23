import { useEffect, useState } from "react";
import FileTypeEnum from "../enum/FileTypeEnum";
import FileData from "../models/FileData";
import FileService from "../services/FileService";

export default function ExampleComponent() {
  const [files, setFiles] = useState<Array<FileData>>();

  useEffect(() => {
    FileService.GetFileData().then((data) => {
      console.log(data);
      setFiles(data);
    });
  }, []);

  return (
    <div>
      {files?.map((file, index) => {
        if (file.Type == FileTypeEnum.Video) {
          return (
            <div key={index}>
              <video width={640} height={360} controls>
                <source
                  src={`${process.env.REACT_APP_BACKEND_URL}/File/GetSingle/${file.RelativeRoute}`}
                  type="video/mp4"
                />
              </video>
            </div>
          );
        }

        return <div key={index}>{file.Name}</div>;
      })}
    </div>
  );
}
