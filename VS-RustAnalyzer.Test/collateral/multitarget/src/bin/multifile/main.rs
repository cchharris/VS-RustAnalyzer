
mod secondfile;

fn main() {
    secondfile::hello_world();
}

#[test]
fn testShouldNotRun() {
    panic!("Should not run")
}
