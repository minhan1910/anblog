import { AuthGuard } from './../../shared/auth.guard';
import { PostCategoryComponent } from './post-categories/post-category.component';
import { PostComponent } from './posts/post.component';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SeriesComponent } from './series/series.component';

const routes: Routes = [
  {
    path: '',
    redirectTo: 'posts',
    pathMatch: 'full',    
  },
  {
    path: 'posts',
    component: PostComponent,
    data: {
      title: 'Posts',
      requiredPolicy: 'Permissions.Posts.View'
    },
    canActivate: [AuthGuard]
  },
  {
    path: 'post-categories',
    component: PostCategoryComponent,
    data: {
      title: 'Post Categories',
      requiredPolicy: 'Permissions.PostCategories.View'
    },
    canActivate: [AuthGuard]
  },
  {
    path: 'series',
    component: SeriesComponent,
    data: {
      title: 'Series',
      requiredPolicy: 'Permissions.Series.View'
    },
    canActivate: [AuthGuard]
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ContentRoutingModule {}
