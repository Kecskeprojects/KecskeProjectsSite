import type IInputForEdit from "./IInputForEdit";

export default interface IWindowDescription {
  title: string;
  inputArray: Array<IInputForEdit>;
  serviceFunction: (data: FormData) => Promise<any>;
  className?: string;
  buttonText?: string;
  hasCloseFunctionality?: boolean;
}
