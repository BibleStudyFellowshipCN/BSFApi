import * as React from 'react';
import { RouteComponentProps } from 'react-router'
import { Link } from 'react-router-dom';
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

        fetch(lessonLink)
            .then(response => response.json() as Promise<Lesson>)
            .then(data => {
                var loading = (this.state.verseLocator.ignoreCase == null);
                this.setState({ lesson: data, loading: loading });
            });
        fetch(patternLink)
            .then(response => response.text() as Promise<string>)
            .then(data => {
                var pattern = "(" + data.replace(/\(/g, "(?:") + ")";
                var loading = (this.state.lesson.culture == null);
                this.setState({ verseLocator: new RegExp(pattern, "g"), loading: loading });
            });
    }

    public render() {
        let contents = this.state.loading
            ? <p><em>Loading...</em></p>
            : this.renderQuestionPage(this.state.lesson);

        return <div>
            {contents}
        </div>;
    }

    private renderQuestionPage(lesson: Lesson) {
        return <div>
            <h1>{this.state.lesson.name}</h1>
            <div className="question">
                <div>背诵经文：<b>{lesson.memoryVerse}</b></div>
                <div>{this.renderDayQuestions(lesson.dayQuestions)}</div>
            </div>
        </div>;
    }

    private renderDayQuestions(days: Day[]) {
        return days.map(
            day => <div>
                <h3>{day.tab} {this.locateVerses(day.title)}</h3>
                <div>{this.renderQuestions(day.questions)}</div>
            </div>
        );
    }

    private renderQuestions(questions: Question[]) {
        return questions.map(
            question => <div>
                <p>{this.locateVerses(question.text)}</p>
                <textarea>{question.answer}</textarea>
            </div>
        );
    }

    private locateVerses(text: string) {
        return text.split(this.state.verseLocator)
            .map((part: string, index: number) => index % 2 === 0 ? part : <Link to={"/Verses/" + part}>{part}</Link>);
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
    text: string;
    answer: string;
}