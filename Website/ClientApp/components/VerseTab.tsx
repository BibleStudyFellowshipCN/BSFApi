import * as React from 'react';
import { RouteComponentProps } from 'react-router'
import 'isomorphic-fetch';

interface FetchVerseDataState {
    verseSet: VerseSet;
    loading: boolean;
}

export class VerseTab extends React.Component<RouteComponentProps<{}>, FetchVerseDataState> {
    constructor() {
        super();
        this.state = {
            verseSet: {} as VerseSet, loading: true
        };
    }

    componentDidMount() {
        this.setState({ loading: false });
    }

    public render() {
        let contents = this.state.loading
            ? <p><em>Loading...</em></p>
            : this.renderQuestionPage(this.state.verseSet);

        return <div>
            <h1>{this.props.match.url}</h1>
            {contents}
        </div>;
    }

    private renderQuestionPage(verseSet: VerseSet) {
        return <div>Coming soon.</div>;
    }
}

interface VerseSet {
    culture: string;
    id: string;
    audio: string;
    name: string;
    memoryVerse: string;
}
