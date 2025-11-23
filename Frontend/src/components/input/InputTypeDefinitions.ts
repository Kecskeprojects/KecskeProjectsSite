import DisabledInput from "./InputDefinitions/DisabledInput";
import FileInput from "./InputDefinitions/FileInput";
import type IInputDefinition from "./InputDefinitions/IInputDefinition";
import MultiFileInput from "./InputDefinitions/MultiFileInput";
import PasswordInput from "./InputDefinitions/PasswordInput";
import TextInput from "./InputDefinitions/TextInput";

export default class InputTypeDefinitions {
  static getType(typeName: string) {
    return InputTypeDefinitions.Types.find((x) => x.name === typeName);
  }

  static Types = new Array<IInputDefinition>(
    new TextInput(),
    new PasswordInput(),
    new FileInput(),
    new MultiFileInput(),
    new DisabledInput()
  );
}
