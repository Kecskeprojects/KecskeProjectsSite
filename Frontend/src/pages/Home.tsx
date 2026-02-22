import { type JSX } from "react";
import { Link } from "react-router-dom";
import InputBase from "../components/input/InputBase";
import Constants from "../enum/Constants";
import InputTypesEnum from "../enum/InputTypesEnum";
import FileService from "../services/FileService";
import LogTools from "../tools/LogTools";

export default function Home(): JSX.Element {
  function PerformUpload(e: React.SyntheticEvent<HTMLFormElement>): void {
    e.preventDefault();
    const formData = new FormData(e.currentTarget);

    FileService.Upload(formData, "", () => {})
      .then(() => {})
      .catch((error) => LogTools.setErrorNotification(error?.message ?? error));
  }

  return (
    <div>
      Home Page
      <div>
        <Link to={`${Constants.FileBaseRoute}/testFolder`}>Example Page</Link>
        <br />

        <form onSubmit={PerformUpload}>
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
      </div>
    </div>
  );
}
