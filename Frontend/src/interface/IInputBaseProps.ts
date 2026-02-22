import type IInputForEdit from "./IInputForEdit";

export default interface IInputBaseProps extends IInputForEdit {
  editedItem: any;
  updatedHandler: (key: string, value: string) => void;
}
