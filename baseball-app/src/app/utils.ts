export class Utils {

    public static formatDateTime(datetime?: string): string {
        if (datetime) {
            return new Date(datetime).toLocaleString();
        } else {
            return '';
        }
    }

    public static formatTime(datetime?: string): string {
        if (datetime) {
            return new Date(datetime).toLocaleTimeString([], { hour: 'numeric', minute: '2-digit' });
        } else {
            return '';
        }
    }
}
