import { useContext, type JSX } from "react";
import { Link } from "react-router-dom";
import InputBase from "../components/input/InputBase";
import { UserContext } from "../Contexts";
import InputTypesEnum from "../enum/InputTypesEnum";
import RoleEnum from "../enum/RoleEnum";
import FileService from "../services/FileService";

export default function Home(): JSX.Element {
  const context = useContext(UserContext);

  function PerformUpload(e: React.SyntheticEvent<HTMLFormElement>): void {
    e.preventDefault();
    const formData = new FormData(e.currentTarget);

    FileService.Upload(formData, "", () => {})
      .then(() => {})
      .catch((error) => console.log(error));
  }

  return (
    <div>
      Home Page
      <div>
        <Link to="example">Example Page</Link>
        <br />
        {context.user?.hasRole(RoleEnum.Admin) ? "IsAdmin" : "IsNotAdmin"}

        <form onSubmit={PerformUpload}>
          <InputBase
            inputType={InputTypesEnum.File}
            label="Add File:"
            name="file"
            editedItem={{}}
            updated={(e) => {
              e.preventDefault();
            }}
          />
          <br />
          <button type="submit">Upload</button>
        </form>
      </div>
    </div>
  );
}
