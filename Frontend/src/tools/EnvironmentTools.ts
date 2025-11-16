export default class EnvironmentTools {
  static getFileSizeLimit(): number {
    const value = import.meta.env.VITE_MAX_FILE_UPLOAD_SIZE_IN_GB;

    const gb = parseFloat(value);

    if (isNaN(gb) || !gb) {
      throw Error("Max file upload size parameter is incorrect!");
    }

    return gb;
  }

  static getBackendRoute(): string {
    const value = import.meta.env.VITE_BACKEND_URL;

    if (!value) {
      throw Error("Backend route parameter is incorrect!");
    }

    return value;
  }

  static IsProduction(): boolean {
    return import.meta.env.MODE === "production";
  }
}
