// angular core
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';

// system module
import { ContentRoutingModule } from './content-routing.module';
import { AnSharedModule } from 'src/app/shared/modules/an-shared.module';
import { PostCategoryDetailComponent } from './post-categories/post-category-detail.component';
import { PostCategoryComponent } from './post-categories/post-category.component';
import { PostActivityLogsComponent } from './posts/post-activity-logs.component';
import { PostSeriesComponent } from './posts/post-series.component';
import { PostReturnReasonComponent } from './posts/post-return-reason.component';
import { SeriesComponent } from './series/series.component';
import { PostDetailComponent } from './posts/post-detail.component';
import { PostComponent } from './posts/post.component';

// primeng
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { PanelModule } from 'primeng/panel';
import { BlockUIModule } from 'primeng/blockui';
import { PaginatorModule } from 'primeng/paginator';
import { BadgeModule } from 'primeng/badge';
import { CheckboxModule } from 'primeng/checkbox';
import { TableModule } from 'primeng/table';
import { KeyFilterModule } from 'primeng/keyfilter';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { IconModule } from '@coreui/icons-angular';
import { ChartjsModule } from '@coreui/angular-chartjs';
import { DropdownModule } from 'primeng/dropdown';
import { InputNumberModule } from 'primeng/inputnumber';
import { ImageModule } from 'primeng/image';
import { DynamicDialogModule } from 'primeng/dynamicdialog';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { EditorModule } from 'primeng/editor';
import { AutoCompleteModule } from 'primeng/autocomplete';
import { SeriesDetailComponent } from './series/series-detail.component';
import { SeriesPostsComponent } from './series/series-posts.component';

@NgModule({
  imports: [
    ContentRoutingModule,
    IconModule,
    CommonModule,
    ReactiveFormsModule,
    ChartjsModule,
    ProgressSpinnerModule,
    PanelModule,
    BlockUIModule,
    PaginatorModule,
    BadgeModule,
    CheckboxModule,
    TableModule,
    KeyFilterModule,
    AnSharedModule,
    ButtonModule,
    InputTextModule,
    InputTextareaModule,
    DropdownModule,
    EditorModule,
    InputNumberModule,
    ImageModule,
    AutoCompleteModule,
    DynamicDialogModule
  ],
  declarations: [
    PostCategoryComponent, 
    PostCategoryDetailComponent,
    PostComponent,
    PostDetailComponent,
    SeriesComponent,
    SeriesDetailComponent,
    PostReturnReasonComponent,
    PostSeriesComponent,
    SeriesPostsComponent,
    PostActivityLogsComponent
  ],
})
export class ContentModule {}
