import InputTypesEnum from "../../../enum/InputTypesEnum";
import type IInputForEdit from "../../../interface/IInputForEdit";
import type IWindowDescription from "../../../interface/IWindowDescription";
import AccountService from "../../../services/AccountService";

export default class LoginWindowDescription implements IWindowDescription {
  title: string;
  serviceFunction: (data: FormData) => Promise<any>;
  inputArray: IInputForEdit[];
  className?: string | undefined;
  buttonText?: string | undefined;
  hasCloseFunctionality?: boolean | undefined;

  constructor() {
    this.title = "Login";
    this.serviceFunction = AccountService.Login;
    this.className = "login";
    this.hasCloseFunctionality = false;
    this.buttonText = "Login";

    this.inputArray = [
      {
        label: "Username",
        name: "username",
        inputType: InputTypesEnum.Text,
      },
      {
        label: "Password",
        name: "password",
        inputType: InputTypesEnum.Password,
      },
    ];
  }
}
