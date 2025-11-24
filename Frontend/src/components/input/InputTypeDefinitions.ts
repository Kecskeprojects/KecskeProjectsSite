import type IInputTypeDefinition from "../../interface/IInputTypeDefinition";
import DisabledInput from "./InputDefinitions/DisabledInput";
import FileInput from "./InputDefinitions/FileInput";
import MultiFileInput from "./InputDefinitions/MultiFileInput";
import PasswordInput from "./InputDefinitions/PasswordInput";
import TextInput from "./InputDefinitions/TextInput";

export default class InputTypeDefinitions {
  static getType(typeName: string) {
    return InputTypeDefinitions.Types.find((x) => x.name === typeName);
  }

  static Types = new Array<IInputTypeDefinition>(
    new TextInput(),
    new PasswordInput(),
    new FileInput(),
    new MultiFileInput(),
    new DisabledInput()
  );
}
