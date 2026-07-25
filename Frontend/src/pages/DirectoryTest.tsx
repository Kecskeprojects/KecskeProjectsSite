import {
  useEffect,
  useMemo,
  useState,
  type JSX,
  type SubmitEvent,
} from "react";
import { Link, useLocation } from "react-router-dom";
import Constants from "../enum/Constants";
import FileTypeEnum from "../enum/FileTypeEnum";
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

    const remainder = rawPath.substring(basePath.length);
    const normalized = remainder.replace(/^\/+|\/+$/g, "");
    return normalized || undefined;
  }, [location.pathname]);

  const [files, setFiles] = useState<Array<FileData>>([]);
  const [directories, setDirectories] = useState<Array<DirectoryData>>([]);
  const [uploadMessage, setUploadMessage] = useState<string | undefined>();
  const [uploadError, setUploadError] = useState<string | undefined>();

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
  }, [targetPath]);

  function buildBackPath(): string | undefined {
    if (!targetPath) {
      return undefined;
    }

    const segments = targetPath.split("/").filter(Boolean);
    if (segments.length <= 1) {
      return `${Constants.FileBaseRoute}`;
    }

    const parentPath = segments.slice(0, -1).join("/");
    return `${Constants.FileBaseRoute}/${parentPath}`;
  }

  async function handleUpload(
    event: SubmitEvent<HTMLFormElement>,
  ): Promise<void> {
    event.preventDefault();
    setUploadMessage(undefined);
    setUploadError(undefined);

    const formData = new FormData(event.currentTarget);
    try {
      const result = await FileService.Upload(formData, targetPath);
      if (result) {
        setUploadMessage(result);
      }
    } catch (error: any) {
      setUploadError(error?.message ?? "Upload failed");
    }
  }

  const backPath = buildBackPath();
  const currentPath = `${Constants.FileBaseRoute}/${targetPath ?? ""}`;

  return (
    <div>
      <h1>Directory Test</h1>
      <div>Current path: {currentPath}</div>
      {backPath ? (
        <div>
          <Link to={backPath}>Back</Link>
        </div>
      ) : null}

      <form onSubmit={handleUpload}>
        <label htmlFor="file-upload">Upload file:</label>
        <input id="file-upload" name="file" type="file" />
        <button type="submit">Upload</button>
      </form>

      {uploadMessage ? <div>{uploadMessage}</div> : null}
      {uploadError ? <div style={{ color: "red" }}>{uploadError}</div> : null}

      <div>
        <h2>Directories</h2>
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
        <h2>Files</h2>
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
