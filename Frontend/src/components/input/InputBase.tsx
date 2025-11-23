import type { JSX } from "react";
import InputTypesEnum from "../../enum/InputTypesEnum";
import InputTypeDefinitions from "./InputTypeDefinitions";

export type InputBaseProps = {
  label: string;
  name: string;
  inputType: string;
  editedItem: any;
  className?: string;
  updated: (event: React.FormEvent<HTMLInputElement>) => void;
  validation?: (inputName: string, editedItem: any) => string;
  isHidden?: (inputName: string, editedItem: any) => boolean;
  isDisabled?: (inputName: string, editedItem: any) => boolean;
};

export default function InputBase(props: InputBaseProps): JSX.Element {
  const isHidden = props.isHidden
    ? props.isHidden(props.name, props.editedItem)
    : false;
  if (isHidden) {
    return <></>;
  }

  const isDisabled = props.isDisabled
    ? props.isDisabled(props.name, props.editedItem)
    : false;
  if (isDisabled) {
    return renderInput(InputTypesEnum.Disabled);
  }

  function renderInput(overrideType?: string): JSX.Element {
    const type = InputTypeDefinitions.getType(overrideType ?? props.inputType);

    if (!type) {
      return <></>;
    }

    return (
      <input
        onInput={props.updated}
        name={props.name}
        type={type.typeText}
        disabled={type.disabled}
        multiple={type.multiple}
      />
    );
  }

  const validationMessage = props.validation
    ? props.validation(props.name, props.editedItem)
    : undefined;

  const additionalClassName = props.className ? ` ${props.className}` : "";

  return (
    <div className={"" + additionalClassName}>
      <label htmlFor={props.name}>{props.label}</label>
      {renderInput()}
      {validationMessage ? <span>{validationMessage}</span> : <></>}
    </div>
  );
}
