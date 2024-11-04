import { environment } from "../environments/environment";
import { LeaderboardPlayer } from './contracts/leaderboard-player';
import { StatDef } from './contracts/stat-def';
import { StatFormat } from './contracts/stat-format';
import { SummaryStat } from './contracts/summary-stat';
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

    public static formatDate(datetime?: string): string {
        if (datetime) {
            return new Date(datetime).toLocaleDateString(undefined, { timeZone: 'UTC' });
        } else {
            return '';
        }
    }

    public static formatTime(datetime?: string): string {
        if (datetime) {
            return new Date(datetime).toLocaleTimeString([], { hour: 'numeric', minute: '2-digit' })
                .replace(' ', '\u00A0');
        } else {
            return '';
        }
    }

    public static fullInningsPitched(stats: { [statName: string]: number }): string {
        return Utils.fullIP(stats['ThirdInningsPitched']);
    }
    public static fullSummaryInningsPitched(summaryStats: SummaryStat[]): string | null {
        const thirds = summaryStats.find(s => s.definition.name === 'ThirdInningsPitched');
        if (thirds && thirds.value) {
            return Utils.fullIP(thirds.value);
        } else {
            return null;
        }
    }
    private static fullIP(thirds: number): string {
        const number = Math.floor(thirds / 3);
        if (number > 0) {
            return number.toString();
        } else if (Utils.partialIP(thirds) === '') {
            return '0';
        } else {
            return '';
        }
    }
    public static partialInningsPitched(stats: { [statName: string]: number }): string {
        return Utils.partialIP(stats['ThirdInningsPitched']);
    }
    public static partialSummaryInningsPitched(summaryStats: SummaryStat[]): string | null {
        const thirds = summaryStats.find(s => s.definition.name === 'ThirdInningsPitched');
        if (thirds && thirds.value) {
            return Utils.partialIP(thirds.value);
        } else {
            return null;
        }
    }
    private static partialIP(thirds: number): string {
        const numerator = thirds % 3;
        if (numerator == 1) {
            return '&frac13;';
        } else if (numerator == 2) {
            return '&frac23;';
        } else {
            return '';
        }
    }

    public static teamColorOrDefault(team: Team): string {
        if (team.colorHex) {
            return `rgb(from #${team.colorHex} r g b / 50%)`;
        } else {
            return 'black';
        }
    }

    public static transparentTeamColor(team: Team, percentage: number): string {
        const baseColor = team.colorHex ? `#${team.colorHex}` : 'black';
        return `rgb(from #${baseColor} r g b / ${percentage}%)`;
    }
}
