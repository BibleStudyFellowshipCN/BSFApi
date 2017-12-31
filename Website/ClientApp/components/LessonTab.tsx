import * as React from 'react';
import { RouteComponentProps } from 'react-router'
import 'isomorphic-fetch';

interface FetchLessonDataState {
    lesson: Lesson;
    verseLocator: RegExp;
    loading: boolean;
}

export class LessonTab extends React.Component<RouteComponentProps<{}>, FetchLessonDataState> {
    constructor() {
        super();
        this.state = {
            lesson: {} as Lesson, verseLocator: {} as RegExp, loading: true
        };
    }

    componentDidMount() {
        const lessonLink = `material/zh-CN${this.props.match.url}`;
        const patternLink = "bible/zh-CN/Verses/Pattern";
        const tempPattern = "((?:创世记|撒母耳记下|列王纪上|列王纪下|历代志上|历代志下|以斯拉记|尼希米记|以斯帖记|约伯记|诗篇|出埃及记|箴言|传道书|雅歌|以赛亚书|耶利米书|耶利米哀歌|以西结书|但以理书|何西阿书|约珥书|利未记|阿摩司书|俄巴底亚书|约拿书|弥迦书|那鸿书|哈巴谷书|西番雅书|哈该书|撒迦利亚书|玛拉基书|民数记|马太福音|马可福音|路加福音|约翰福音|使徒行传|罗马书|哥林多前书|哥林多后书|加拉太书|以弗所书|申命记|腓立比书|歌罗西书|帖撒罗尼迦前书|帖撒罗尼迦后书|提摩太前书|提摩太后书|提多书|腓利门书|希伯来书|雅各书|约书亚记|彼得前书|彼得后书|约翰一书|约翰二书|约翰三书|犹大书|启示录|士师记|路得记|撒母耳记上)(?:(?:[;；]* *(?:(?:\\d+) *[:：](?: *\\d+ *(?:(?:[-] *\\d+ *([:：] *\\d+)?)?)?))(?: *[,，、](?:(?: *\\d+ *(?:(?:[-] *\\d+ *([:：] *\\d+)?)?)?)))*)+))";

        fetch(lessonLink)
            .then(response => response.json() as Promise<Lesson>)
            .then(data => {
                this.setState({ lesson: data, verseLocator: new RegExp(tempPattern, "g"), loading: false });
            });
        ////fetch(patternLink)
        ////    .then(response => response.json() as Promise<string>)
        ////    .then(data => {
        ////        this.setState({ versePattern: data });
        ////    });
    }

    public render() {
        let contents = this.state.loading
            ? <p><em>Loading...</em></p>
            : this.renderQuestionPage(this.state.lesson);

        return <div>
            <h1>{this.state.lesson.name} {this.state.lesson.id}</h1>
            {contents}
        </div>;
    }

    private renderQuestionPage(lesson: Lesson) {
        return <div className="question">
            <p>背诵经文：{lesson.memoryVerse}</p>
            <div>{this.renderDayQuestions(lesson.dayQuestions)}</div>
        </div>;
    }

    private renderDayQuestions(days: Day[]) {
        return days.map(
            day => <div>
                <p>{day.tab} {this.locateVerses(day.title)}</p>
                <div>{this.renderQuestions(day.questions)}</div>
            </div>
        );
    }

    private renderQuestions(questions: Question[]) {
        return questions.map(
            question => <div>
                <p>{this.locateVerses(question.questionText)}</p>
                <textarea>{question.answer}</textarea>
            </div>
        );
    }

    private locateVerses(text: string) {
        return text.split(this.state.verseLocator)
            .map((part: string, index: number) => index % 2 === 0 ? part : <a href={part} target="_blank">{part}</a>);
    }
}

interface Lesson {
    culture: string;
    id: string;
    audio: string;
    name: string;
    memoryVerse: string;
    dayQuestions: Day[];
}

interface Day {
    tab: string;
    title: string;
    questions: Question[];
}

interface Question {
    id: string;
    questionText: string;
    answer: string;
}