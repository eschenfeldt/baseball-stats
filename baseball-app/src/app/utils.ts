import { environment } from "../environments/environment";
import { LeaderboardPlayer } from './contracts/leaderboard-player';
import { StatDef } from './contracts/stat-def';
import { StatFormat } from './contracts/stat-format';
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

    public static fullInningsPitched(stats: { [statName: string]: number }): string {
        const number = Math.floor(stats['ThirdInningsPitched'] / 3);
        if (number > 0) {
            return number.toString();
        } else if (Utils.partialInningsPitched(stats) === '') {
            return '0';
        } else {
            return '';
        }
    }
    public static partialInningsPitched(stats: { [statName: string]: number }): string {
        const numerator = stats['ThirdInningsPitched'] % 3;
        if (numerator == 1) {
            return '&frac13;';
        } else if (numerator == 2) {
            return '&frac23;';
        } else {
            return '';
        }
    }

    public static transparentTeamColor(team: Team, percentage: number): string {
        const baseColor = team.colorHex ? `#${team.colorHex}` : 'black';
        return `rgb(from #${baseColor} r g b / ${percentage}%)`;
    }
}
