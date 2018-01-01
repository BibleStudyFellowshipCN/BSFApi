import * as React from 'react';
import { RouteComponentProps } from 'react-router';
import { Link } from 'react-router-dom';
import 'isomorphic-fetch';

interface FetchStudyDataState {
    studies: Study;
    loading: boolean;
}

export class StudyTab extends React.Component<RouteComponentProps<{}>, FetchStudyDataState> {
    constructor() {
        super();
        this.state = {studies: {} as Study, loading: true };
    }

    componentDidMount() {
        fetch('material/zh-CN/Studies/罗马书 2017-2018')
            .then(response => response.json() as Promise<Study>)
            .then(data => {
                this.setState({ studies: data, loading: false });
            });
    }

    public render() {
        let contents = this.state.loading
            ? <p><em>Loading...</em></p>
            : StudyTab.renderStudyTable(this.state.studies);

        return <div>
            <h1>Study list</h1>
            { contents }
        </div>;
    }

    private static renderStudyTable(study: Study) {
        return <div>
            <p>{study.title}</p>
            <table className='table'>
                <thead>
                    <tr>
                        <th>预设日期</th>
                        <th>序号</th>
                    </tr>
                </thead>
                <tbody> {}
                    {study.lessons.sort(function (x, y) {
                        return x.proposedDate < y.proposedDate ? 1 : x.proposedDate > y.proposedDate ? -1 : 0;
                    }).map(StudyTab.renderLesson)}
                </tbody>
            </table>
        </div>;
    }

    private static renderLesson(lesson: LessonItem) {
        const questionLink = `Lessons/${lesson.id}`;
        return <tr>
            <td>{lesson.proposedDate}</td>
            <td>{lesson.name} 经文释义 <Link to={questionLink}>成人研经题</Link></td>
        </tr>;
    }
}

interface Study {
    culture: string;
    title: number;
    lessons: LessonItem[];
}

interface LessonItem {
    id: string;
    name: string;
    proposedDate: string;
}