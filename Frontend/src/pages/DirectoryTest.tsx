import {
  useEffect,
  useMemo,
  useState,
  type JSX,
  type SubmitEvent,
} from "react";
import { Link, useLocation } from "react-router-dom";
import InputBase from "../components/input/InputBase";
import Constants from "../enum/Constants";
import FileTypeEnum from "../enum/FileTypeEnum";
import InputTypesEnum from "../enum/InputTypesEnum";
import type DirectoryData from "../models/DirectoryData";
import FileData from "../models/FileData";
import FileService from "../services/FileService";
import LogTools from "../tools/LogTools";

export default function DirectoryTest(): JSX.Element {
  const location = useLocation();
  const targetPath = useMemo(() => {
    const rawPath = location.pathname;
    const basePath = `${Constants.FileBaseRoute}/`;
    if (!rawPath.startsWith(basePath)) {
      return undefined;
    }

    const sanitizedPath = rawPath.substring(basePath.length);
    return sanitizedPath.length ? sanitizedPath : undefined;
  }, [location.pathname]);

  const [files, setFiles] = useState<Array<FileData>>([]);
  const [directories, setDirectories] = useState<Array<DirectoryData>>([]);
  const [refreshToggle, setRefreshToggle] = useState<boolean>(false);

  useEffect(() => {
    if (!targetPath) {
      setDirectories([]);
      setFiles([]);
      return;
    }

    FileService.GetDirectoryData(targetPath)
      .then((data) => setDirectories(data))
      .catch((error) => LogTools.setErrorNotification(error?.message ?? error));

    FileService.GetFileData(targetPath)
      .then((data) => setFiles(data))
      .catch((error) => LogTools.setErrorNotification(error?.message ?? error));

    return () => {
      setDirectories([]);
      setFiles([]);
      setRefreshToggle(false);
    };
  }, [targetPath, refreshToggle]);

  function buildBackPath(): string | undefined {
    if (!targetPath) {
      return undefined;
    }

    const segments = targetPath.split("/");
    if (segments.length <= 1) {
      return undefined;
    }

    const parentPath = segments.slice(0, -1).join("/");
    return `${Constants.FileBaseRoute}/${parentPath}`;
  }

  function handleUpload(e: SubmitEvent<HTMLFormElement>): void {
    e.preventDefault();
    const formData = new FormData(e.currentTarget);

    FileService.Upload(formData, "testFolder")
      .then(() => {
        setRefreshToggle(true);
      })
      .catch((error) => LogTools.setErrorNotification(error?.message ?? error));
  }

  const backPath = buildBackPath();
  const currentPath = `${Constants.FileBaseRoute}/${targetPath ?? ""}`;

  return (
    <div>
      <h3>Directory Test</h3>
      <div>Current path: {currentPath}</div>
      {backPath ? (
        <div>
          <Link to={backPath}>Back</Link>
          <br />
        </div>
      ) : null}

      <h3>Upload File</h3>
      <form onSubmit={handleUpload}>
        <InputBase
          inputType={InputTypesEnum.File}
          label="Add File:"
          name="file"
          editedItem={{}}
          updatedHandler={() => {}}
        />
        <br />
        <button type="submit">Upload</button>
      </form>

      <div>
        <h3>Directories</h3>
        {directories.length === 0 ? <div>No subdirectories</div> : null}
        {directories.map((directory, index) => (
          <div key={`directory-${index}`}>
            <Link to={`${Constants.FileBaseRoute}/${directory.SubPath}`}>
              {directory.Name}
            </Link>
          </div>
        ))}
      </div>

      <div>
        <h3>Files</h3>
        {files.length === 0 ? <div>No files</div> : null}
        {files.map((file, index) => {
          if (file.Type === FileTypeEnum.Video) {
            return (
              <div key={`file-${index}`}>
                <video width={640} height={360} controls>
                  <source
                    src={FileService.GetSingleFileEndpoint(
                      targetPath,
                      file.Identifier,
                    )}
                    type="video/mp4"
                  />
                </video>
              </div>
            );
          }

          return <div key={`file-${index}`}>{file.Name}</div>;
        })}
      </div>
    </div>
  );
}
