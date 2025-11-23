import InputTypesEnum from "../../../enum/InputTypesEnum";
import type IInputDefinition from "./IInputDefinition";

export default class FileInput implements IInputDefinition {
  name: string;
  typeText: string;

  constructor() {
    this.name = InputTypesEnum.File;
    this.typeText = InputTypesEnum.File;
  }
}
