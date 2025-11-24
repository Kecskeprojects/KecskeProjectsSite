import type IWindowInput from "./IWindowInput";

export default interface IWindowDescription {
  title: string;
  inputArray: Array<IWindowInput>;
  serviceFunction: (data: FormData) => Promise<any>;
  className?: string;
  buttonText?: string;
  hasCloseFunctionality?: boolean;
}
