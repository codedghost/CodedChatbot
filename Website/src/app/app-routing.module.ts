import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { RouterModule, Routes } from "@angular/router";
import { SongsListComponent } from "./songs-list/songs-list.component";

const routes: Routes = [{ path: "songs-list", component: SongsListComponent }];

@NgModule({
  imports: [CommonModule, RouterModule.forRoot(routes)],
  exports: [RouterModule],

  declarations: []
})
export class AppRoutingModule {}
