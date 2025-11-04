import { useEffect, useState, type JSX } from "react";
import { useParams } from "react-router-dom";
import FileTypeEnum from "../enum/FileTypeEnum";
import FileData from "../models/FileData";
import FileService from "../services/FileService";

export default function ExampleComponent(): JSX.Element {
  const { id } = useParams();
  const [files, setFiles] = useState<Array<FileData>>();

  useEffect(() => {
    FileService.GetFileData().then((data) => {
      setFiles(data);
    });
  }, []);

  return (
    <div>
      <div>ID: {id}</div>
      {files?.map((file, index) => {
        if (file.Type == FileTypeEnum.Video) {
          return (
            <div key={index}>
              <video width={640} height={360} controls>
                <source
                  src={FileService.GetSingleFileEndpoint(
                    file.Identifier,
                    file.Folder
                  )}
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
