import * as React from 'react';
import { Route } from 'react-router-dom';
import { Layout } from './components/Layout';
import { Home } from './components/Home';
import { LessonTab } from './components/LessonTab';
import { StudyTab } from './components/StudyTab';

export const routes = <Layout>
    <Route exact path='/' component={ Home } />
    <Route path='/Study' component={StudyTab} />
    <Route path='/Lessons/:lessonId' component={LessonTab} />
</Layout>;
