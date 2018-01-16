import * as React from 'react';
import { RouteComponentProps } from 'react-router'
import 'isomorphic-fetch';

interface FetchVerseDataState {
    chapters: Chapter[];
    loading: boolean;
}

export class VerseTab extends React.Component<RouteComponentProps<{}>, FetchVerseDataState> {
    constructor() {
        super();
        this.state = {
            chapters: [] as Chapter[], loading: true
        };
    }

    componentDidMount() {
        const lessonLink = `bible/zh-CN${this.props.match.url}`;

        fetch(lessonLink)
            .then(response => response.json() as Promise<Chapter[]>)
            .then(data => {
                this.setState({ chapters: data, loading: false });
            });
    }

    public render() {
        let contents = this.state.loading
            ? <p><em>Loading...</em></p>
            : this.renderVersePage(this.state.chapters);

        return <div>
            <h1>{this.props.match.url}</h1>
            {contents}
        </div>;
    }

    private renderVersePage(chapters: Chapter[]) {
        return <div>{chapters.map(this.renderChapter)}</div>;
    }

    private renderChapter(chapter: Chapter) {
        return <div>
            <h2>Version: {chapter.version}, Chapter: {chapter.order}</h2>
            {chapter.verses.map(verse => <p>{verse.order} {verse.text}</p>)}
        </div>;
    }
}

interface Chapter {
    version: string;
    bookOrder: number;
    order: number;
    culture: string;
    verses: Verse[];
}

interface Verse {
    order: number;
    text: string;
}
