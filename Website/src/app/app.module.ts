import { BrowserModule } from "@angular/platform-browser";
import { NgModule } from "@angular/core";
import { MatInputModule, MatToolbarModule, MatButtonModule, MatTableModule, MatSortModule } from "@angular/material";
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";

import { AppComponent } from "./app.component";
import { AppRoutingModule } from './app-routing.module';
import { SongsListComponent } from './songs-list/songs-list.component';

@NgModule({
  declarations: [AppComponent, SongsListComponent],
  imports: [
    BrowserModule,
    MatInputModule,
    MatToolbarModule,
    MatButtonModule,
    MatTableModule,
    MatSortModule,
    BrowserAnimationsModule,
    AppRoutingModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule {}
