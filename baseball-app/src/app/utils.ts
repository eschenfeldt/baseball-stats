import { environment } from "../environments/environment";
import { StatDef } from './contracts/stat-def';
import { Team } from "./contracts/team";

export class Utils {

    public static keyToUrl(key: string): string {
        return `${environment.bucketUrl}/${key}`;
    }

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

    public static formatString(statDef: StatDef): string {
        if (statDef.format.name === 'Decimal') {
            return `0.3`; // TODO: rework the format object to get this to actually work
        } else {
            return '';
        }
    }

    public static transparentTeamColor(team: Team, percentage: number): string {
        const baseColor = team.colorHex ? `#${team.colorHex}` : 'black';
        return `rgb(from #${baseColor} r g b / ${percentage}%)`;
    }
}
