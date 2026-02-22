import type { JSX } from "react";
import InputBase from "../components/input/InputBase";
import InputTypesEnum from "../enum/InputTypesEnum";
import SecurityService from "../services/SecurityService";
import LogTools from "../tools/LogTools";

export default function Security(): JSX.Element {
  function submitIP(
    e: React.SyntheticEvent<HTMLButtonElement>,
    checkInput?: boolean,
  ) {
    e.preventDefault();

    let inputValue;
    if (checkInput) {
      const elements = document.getElementsByName("ipaddress");
      if (elements.length > 0 && elements[0] instanceof HTMLInputElement) {
        inputValue = elements[0].value;
      }
    }

    SecurityService.AddAddressToRule(inputValue)
      .then((result) => {
        if (result.message) {
          LogTools.setSuccessNotification(result.message);
        }
      })
      .catch((error) => LogTools.setErrorNotification(error?.message ?? error));
  }

  return (
    <div>
      <InputBase
        inputType={InputTypesEnum.Text}
        label="IP Address:"
        name="ipaddress"
        editedItem={{}}
        updatedHandler={() => {}}
      />
      <br />
      <button type="button" onClick={(e) => submitIP(e, true)}>
        Add IP
      </button>
      <button type="button" onClick={(e) => submitIP(e, false)}>
        Add My IP
      </button>
    </div>
  );
}
