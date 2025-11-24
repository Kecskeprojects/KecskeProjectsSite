export default interface IWindowInput {
  label: string;
  name: string;
  inputType: string;
  className?: string;
  validation?: (inputName: string, editedItem: any) => string;
  isHidden?: (inputName: string, editedItem: any) => boolean;
  isDisabled?: (inputName: string, editedItem: any) => boolean;
}
