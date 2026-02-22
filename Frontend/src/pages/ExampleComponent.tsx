import { useEffect, useState, type JSX } from "react";
import { Link, useParams } from "react-router-dom";
import FileTypeEnum from "../enum/FileTypeEnum";
import type DirectoryData from "../models/DirectoryData";
import FileData from "../models/FileData";
import FileService from "../services/FileService";

export default function ExampleComponent(): JSX.Element {
  const { category, subPath } = useParams();
  const [files, setFiles] = useState<Array<FileData>>();
  const [directories, setDirectories] = useState<Array<DirectoryData>>();

  useEffect(() => {
    FileService.GetDirectoryData(category, subPath).then((data) => {
      setDirectories(data);
    });
    FileService.GetFileData(category, subPath).then((data) => {
      setFiles(data);
    });
  }, [category, subPath]);

  const backPath =
    "/files" +
    `/${category}` +
    (!subPath || subPath?.lastIndexOf(">") === -1
      ? ""
      : `/${subPath?.substring(0, subPath.lastIndexOf(">"))}`);

  return (
    <div>
      <div>subPath: {subPath}</div>
      {subPath ? (
        <>
          <Link to={backPath}>Back</Link>
          <br />
        </>
      ) : null}
      {directories?.map((directory, index) => {
        return (
          <Link to={directory.SubPath} key={"directory-" + index}>
            {directory.Name}
          </Link>
        );
      })}
      {files?.map((file, index) => {
        if (file.Type == FileTypeEnum.Video) {
          return (
            <div key={"file-" + index}>
              <video width={640} height={360} controls>
                <source
                  src={FileService.GetSingleFileEndpoint(
                    category,
                    file.SubPath,
                    file.Identifier,
                  )}
                  type="video/mp4"
                />
              </video>
            </div>
          );
        }

        return <div key={"file-" + index}>{file.Name}</div>;
      })}
    </div>
  );
}
