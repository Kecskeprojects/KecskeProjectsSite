import InputTypesEnum from "../../../enum/InputTypesEnum";
import type IInputDefinition from "./IInputDefinition";

export default class MultiFileInput implements IInputDefinition {
  name: string;
  typeText: string;
  multiple?: boolean | undefined;

  constructor() {
    this.name = InputTypesEnum.MultiFile;
    this.typeText = InputTypesEnum.File;
    this.multiple = true;
  }
}
