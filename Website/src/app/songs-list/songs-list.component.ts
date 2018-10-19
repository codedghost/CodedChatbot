import { ISongsListSong } from "./songs-list.model";
import { Component, OnInit, Input, ViewChild } from "@angular/core";
import { MatSort, MatTableDataSource } from "@angular/material";

const SONGS = [
  {
    name: "tornado of souls",
    artist: "megadeth",
    isPaidDLC: true
  },
  {
    name: "holy wars",
    artist: "megadeth",
    isPaidDLC: true
  }
];

@Component({
  selector: "app-songs-list",
  templateUrl: "./songs-list.component.html",
  styleUrls: ["./songs-list.component.css"]
})
export class SongsListComponent implements OnInit {
  datasource = new MatTableDataSource(SONGS);
  displayedColumns = ["name", "artist"];

  @ViewChild(MatSort)
  sort: MatSort;
  searchText;
  constructor() {}

  ngOnInit() {
    this.datasource.sort = this.sort;
    
  }

  applyFilter(filterValue: string) {
    this.datasource.filter = filterValue.trim().toLowerCase();
  }
}
